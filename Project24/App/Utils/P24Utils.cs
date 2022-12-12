/*  P24Utils.cs
 *  Version: 1.0 (2022.11.28)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Project24.App;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.Identity;

namespace Project24.Utils.ClinicManager
{
    public enum P24Module : byte
    {
        Unset = 0,
        Customer,
        Ticket
    }

    public static class P24Utils
    {
        public static Tuple<string, string> SplitFirstLastName(string _fullName)
        {
            if (string.IsNullOrEmpty(_fullName))
                return null;

            string[] tokens = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1)
                return new Tuple<string, string>(null, tokens[0].Trim());

            string middlename = "";
            for (int i = 0; i < tokens.Length - 1; ++i)
            {
                middlename += tokens[i].Trim() + " ";
            }

            return new Tuple<string, string>(middlename.Trim(), tokens[^1].Trim());
        }
    }

    public class ImageProcessor
    {
        public Dictionary<string, string> CustomInfo { get; private set; }


        public ImageProcessor(ApplicationDbContext _dbContext, P24IdentityUser _currentUser, CustomerProfile _customer)
        {
            m_DbContext = _dbContext;
            m_CurrentUser = _currentUser;
            m_Customer = _customer;

            CustomInfo = new Dictionary<string, string>()
            {
                {  CustomInfoKey.CustomerCode, _customer.Code }
            };
        }

        public async Task<bool> ProcessDelete(CustomerImage _image)
        {
            CustomInfo[CustomInfoKey.ImageId] = _image.Id.ToString();
            
            string absPathDeleted = DriveUtils.DeletedDataRootPath + "/" + _image.Path;
            Directory.CreateDirectory(absPathDeleted);

            string srcFile = DriveUtils.DataRootPath + "/" + _image.Path + "/" + _image.Name;
            string dstFile = absPathDeleted + "/" + _image.Name;

            try
            {
                File.Move(srcFile, dstFile, true);
            }
            catch (Exception _e)
            {
                CustomInfo[CustomInfoKey.Error] = "";
                CustomInfo[CustomInfoKey.Message] = _e.Message;

                return false;
            }

            if (File.Exists(srcFile))
                File.Delete(srcFile);

            _image.DeletedDate = DateTime.Now;
            _image.UpdatedUser = m_CurrentUser;
            m_DbContext.Update(_image);

            m_Customer.UpdatedDate = DateTime.Now;
            m_Customer.UpdatedUser = m_CurrentUser;
            m_DbContext.Update(m_CurrentUser);

            return true;
        }

        public async Task<string> ProcessUpload(IFormFile[] _fileUploads)
        {
            await SaveFiles(_fileUploads);

            m_Customer.UpdatedDate = DateTime.Now;
            m_Customer.UpdatedUser = m_CurrentUser;

            string operationStatus = ActionRecord.OperationStatus_.Success;

            if (CustomInfo.ContainsKey(CustomInfoKey.AddedList))
            {
                operationStatus += ", " + ActionRecord.OperationStatus_.HasUpload;
            }
            if (CustomInfo.ContainsKey(CustomInfoKey.Malfunctions))
            {
                operationStatus += ", " + ActionRecord.OperationStatus_.HasMalfunction;
            }

            return operationStatus;
        }

        private async Task SaveFiles(IFormFile[] _fileUploads)
        {
            List<CustomerImage> images = new List<CustomerImage>();
            string addedList = "";
            string malfunctions = "";

            foreach (var file in _fileUploads)
            {
                string[] contentType = file.ContentType.Split('/');
                if (contentType[0] != "image")
                {
                    malfunctions += file.ContentType + ", ";
                    continue;
                }

                string absPath = DriveUtils.DataRootPath + "/" + m_Customer.Code;
                Directory.CreateDirectory(absPath);

                FileStream stream = File.Create(absPath + "/" + file.FileName);
                file.CopyTo(stream);
                stream.Close();

                // TODO: check file signature;

                string path = m_Customer.Code + "/" + file.FileName;
                CustomerImage image = new CustomerImage(m_CurrentUser, m_Customer)
                {
                    Path = m_Customer.Code,
                    Name = file.FileName
                };
                images.Add(image);

                addedList += file.FileName + ", ";
            }

            m_DbContext.AddRange(images);

            char[] trimChars = new char[] { ',', ' ' };

            CustomInfo[CustomInfoKey.AddedList] = addedList.TrimEnd(trimChars);
            if (malfunctions != "")
                CustomInfo[CustomInfoKey.Malfunctions] = malfunctions.TrimEnd(trimChars);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly P24IdentityUser m_CurrentUser;
        private readonly CustomerProfile m_Customer;
    }
}
