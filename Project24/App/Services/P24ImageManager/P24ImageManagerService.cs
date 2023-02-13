/*  P24ImageManagerService.cs
 *  Version: 1.4 (2023.02.14)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.Identity;

namespace Project24.App.Services.P24ImageManager
{
    public class P24ImageManagerService
    {
        private class RequestData
        {
            public readonly P24IdentityUser User;
            public readonly P24ObjectBase Entity;

            public RequestData(P24IdentityUser _user, P24ObjectBase _entity)
            {
                User = _user;
                Entity = _entity;
            }
        }

        public class ResponseData
        {
            public bool IsSuccess { get; set; } = false;
            public string LastMessage { get; set; }

            public List<string> InvalidFileNames { get; set; } = new List<string>();
            public List<string> AddedFileNames { get; set; } = new List<string>();
            public List<string> DeletedFileNames { get; set; } = new List<string>();
            public List<string> ErrorFileMessages { get; set; } = new List<string>();
        }


        public P24ImageManagerService(ApplicationDbContext _dbContext, ILogger<P24ImageManagerService> _logger)
        {
            m_DbContext = _dbContext;
            m_Logger = _logger;
        }


        /// <summary> Process image upload for the current entity, namely CustomerProfile (customer) and VisitingProfile (visiting ticket). </summary>
        /// <param name="_user">The user performing the request.</param>
        /// <param name="_entity">The entity to be attached to.</param>
        /// <param name="_files">The list of files to be processed.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        public async Task<ResponseData> UploadAsync(P24IdentityUser _user, P24ObjectBase _entity, IFormFile[] _files, CancellationToken _cancellationToken = default)
        {
            m_RequestData = new RequestData(_user, _entity);
            m_ResponseData = new ResponseData();

            if (_entity is CustomerProfile)
            {
                await UploadCustomerImageAsync(_files, _cancellationToken);
                m_ResponseData.IsSuccess = true;

                return m_ResponseData;
            }

            if (_entity is TicketProfile)
            {
                await UploadTicketImageAsync(_files, _cancellationToken);
                m_ResponseData.IsSuccess = true;

                return m_ResponseData;
            }

            m_Logger.LogWarning("Something happened in UploadAsync: control reached method end.");
            return null;
        }

        /// <summary> Delete an images from database and move them to deleted storage. </summary>
        /// <param name="_user">The user performing the request.</param>
        /// <param name="_image">The image to be deletd.</param>
        /// <param name="_newName">New name that the image will be renamed to.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        /// <returns></returns>
        public ResponseData Rename(P24IdentityUser _user, P24ImageModelBase _image, string _newName, CancellationToken _cancellationToken = default)
        {
            m_RequestData = new RequestData(_user, null);
            m_ResponseData = new ResponseData();

            if (RenameImage(_image, _newName, _cancellationToken))
            {
                _image.Name = _newName;
                _image.UpdatedUser = m_RequestData.User;

                m_DbContext.Update(_image);
                m_ResponseData.IsSuccess = true;
            }
            else
            {
                m_ResponseData.IsSuccess = false;
            }

            return m_ResponseData;
        }

        /// <summary> Delete a list of images from database and move them to deleted storage. </summary>
        /// <typeparam name="T">The image entity type, namely `CustomerImage` and `TicketImage`.</typeparam>
        /// <param name="_user">The user performing the request.</param>
        /// <param name="_images">The list of images to be deletd.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        public ResponseData Delete<T>(P24IdentityUser _user, ICollection<T> _images, CancellationToken _cancellationToken = default)
            where T : P24ImageModelBase
        {
            m_RequestData = new RequestData(_user, null);
            m_ResponseData = new ResponseData();

            foreach (var image in _images)
            {
                if (DeleteImage(image, _cancellationToken))
                {
                    image.DeletedDate = DateTime.Now;
                    image.UpdatedUser = m_RequestData.User;

                    m_DbContext.Update(image);
                }
            }
            m_ResponseData.IsSuccess = true;

            return m_ResponseData;
        }

        /// <summary> Delete an images from database and move them to deleted storage. </summary>
        /// <param name="_user">The user performing the request.</param>
        /// <param name="_image">The image to be deletd.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        /// <returns></returns>
        public ResponseData Delete(P24IdentityUser _user, P24ImageModelBase _image, CancellationToken _cancellationToken = default)
        {
            m_RequestData = new RequestData(_user, null);
            m_ResponseData = new ResponseData();

            if (DeleteImage(_image, _cancellationToken))
            {
                _image.DeletedDate = DateTime.Now;
                _image.UpdatedUser = m_RequestData.User;

                m_DbContext.Update(_image);
            }
            m_ResponseData.IsSuccess = true;

            return m_ResponseData;
        }

        #region Upload
        /// <summary> Process image upload for CustomerImage. </summary>
        /// <param name="_files">List of files to be processed.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        private async Task UploadCustomerImageAsync(IFormFile[] _files, CancellationToken _cancellationToken)
        {
            var validFiles = ValidateFiles(_files);

            CustomerProfile customer = m_RequestData.Entity as CustomerProfile;
            List<CustomerImage> addedImages = new List<CustomerImage>(validFiles.Count);

            long totalSize = 0L;

            string path = customer.Code;
            string absPath = DriveUtils.DataRootPath + "/" + path;
            Directory.CreateDirectory(absPath);

            foreach (var file in _files)
            {
                if (!await SaveFileAsync(file, absPath))
                    continue;

                // TODO: check file signature;

                CustomerImage image = new CustomerImage(m_RequestData.User, customer)
                {
                    Path = customer.Code,
                    Name = file.FileName
                };
                addedImages.Add(image);

                m_ResponseData.AddedFileNames.Add(file.FileName);
                totalSize += file.Length;
            }

            UserUpload upload = new UserUpload(m_RequestData.User, AppModule.P24_ClinicManager, addedImages.Count, totalSize);

            await m_DbContext.AddRangeAsync(addedImages);
            await m_DbContext.AddAsync(upload);
        }

        /// <summary> Process image upload for TicketImage. </summary>
        /// <param name="_files">List of files to be processed.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        private async Task UploadTicketImageAsync(IFormFile[] _files, CancellationToken _cancellationToken)
        {
            var validFiles = ValidateFiles(_files);

            TicketProfile ticket = m_RequestData.Entity as TicketProfile;
            CustomerProfile customer = ticket.Customer;
            List<TicketImage> addedImages = new List<TicketImage>(validFiles.Count);

            long totalSize = 0L;

            string path = customer.Code + "/" + ticket.Code;
            string absPath = DriveUtils.DataRootPath + "/" + path;
            Directory.CreateDirectory(absPath);

            foreach (var file in _files)
            {
                if (!await SaveFileAsync(file, absPath))
                    continue;

                // TODO: check file signature;

                TicketImage image = new TicketImage(m_RequestData.User, ticket)
                {
                    Path = path,
                    Name = file.FileName
                };
                addedImages.Add(image);

                m_ResponseData.AddedFileNames.Add(file.FileName);
                totalSize += file.Length;
            }

            UserUpload upload = new UserUpload(m_RequestData.User, AppModule.P24_ClinicManager, addedImages.Count, totalSize);

            await m_DbContext.AddRangeAsync(addedImages);
            await m_DbContext.AddAsync(upload);
        }

        /// <summary> Save a single IFormFile file to disk at the specified path. </summary>
        /// <param name="_file">The IFormFile file to save.</param>
        /// <param name="_absPath">The absolute path to where to save the file.</param>
        /// <returns>true if the operation success, otherwise false.</returns>
        private async Task<bool> SaveFileAsync(IFormFile _file, string _absPath)
        {
            try
            {
                FileStream stream = File.Create(_absPath + "/" + _file.FileName);
                await _file.CopyToAsync(stream);
                stream.Close();

                return true;
            }
            catch (Exception _e)
            {
                m_ResponseData.ErrorFileMessages.Add(_file.FileName + ": " + _e.Message);
                return false;
            }
        }

        /// <summary> Validates uploaded files. Only image files are allowed. </summary>
        /// <param name="_files">The list of IFormFile files to be validated.</param>
        /// <returns>A list of accepted image files.</returns>
        private List<IFormFile> ValidateFiles(IFormFile[] _files)
        {
            List<IFormFile> validFiles = new List<IFormFile>();

            foreach (var file in _files)
            {
                string[] contentType = file.ContentType.Split('/');
                if (contentType[0] != "image")
                {
                    m_ResponseData.InvalidFileNames.Add(file.FileName);
                    continue;
                }

                validFiles.Add(file);
            }

            return validFiles;
        }
        #endregion

        #region Delete
        /// <summary> "Delete an image by moving the physical file to deleted storage. </summary>
        /// <param name="_image">The image to be deleted.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        /// <returns>true if the operation success, otherwise false.</returns>
        private bool DeleteImage(P24ImageModelBase _image, CancellationToken _cancellationToken)
        {
            string srcPath = DriveUtils.DataRootPath + "/" + _image.FullName;
            string dstPath = DriveUtils.DeletedDataRootPath + "/" + _image.Path;
            Directory.CreateDirectory(dstPath);

            dstPath += "/" + _image.Name;

            try
            {
                if (!File.Exists(srcPath))
                {
                    m_ResponseData.ErrorFileMessages.Add(_image.Name + ": file doesn't exist");
                    return false;
                }

                File.Move(srcPath, dstPath, true);
                if (!File.Exists(dstPath) || File.Exists(srcPath))
                {
                    m_ResponseData.ErrorFileMessages.Add(_image.Name + ": move failed");
                    return false;
                }
            }
            catch (Exception _e)
            {
                m_ResponseData.ErrorFileMessages.Add(_image.Name + ": " + _e.Message);
                return false;
            }

            m_ResponseData.DeletedFileNames.Add(_image.Name);
            return true;
        }
        #endregion

        #region Rename
        /// <summary> "Delete an image by moving the physical file to deleted storage. </summary>
        /// <param name="_image">The image to be deleted.</param>
        /// <param name="_newName">New name that the image will be renamed to.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        /// <returns>true if the operation success, otherwise false.</returns>
        private bool RenameImage(P24ImageModelBase _image, string _newName, CancellationToken _cancellationToken)
        {
            string srcPath = DriveUtils.DataRootPath + "/" + _image.FullName;
            string dstPath = DriveUtils.DataRootPath + "/" + _image.Path + "/" + _newName;

            try
            {
                if (!File.Exists(srcPath))
                {
                    m_ResponseData.ErrorFileMessages.Add(_image.Name + ": file doesn't exist");
                    m_ResponseData.LastMessage = "File gốc không tồn tại.";
                    return false;
                }

                if (File.Exists(dstPath))
                {
                    m_ResponseData.ErrorFileMessages.Add(_image.Name + ": file with new name (" + _newName + ") already existed");
                    m_ResponseData.LastMessage = "File <code>" + _newName + "</code> bị trùng tên.";
                    return false;
                }

                File.Move(srcPath, dstPath);
                if (File.Exists(srcPath))
                {
                    m_ResponseData.ErrorFileMessages.Add(_image.Name + ": rename failed");
                    m_ResponseData.LastMessage = "Không đổi tên được file.";
                    return false;
                }
            }
            catch (Exception _e)
            {
                m_ResponseData.ErrorFileMessages.Add(_image.Name + ": " + _e.Message);
                m_ResponseData.LastMessage = "Lỗi hệ thống.";
                return false;
            }

            m_ResponseData.AddedFileNames.Add(_image.Name);
            return true;
        }
        #endregion


        private RequestData m_RequestData;
        private ResponseData m_ResponseData;

        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<P24ImageManagerService> m_Logger;
    }

}
