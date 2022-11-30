/*  P24ImageManagerService.cs
 *  Version: 1.0 (2022.11.30)
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
using Project24.Identity;
using Project24.Models.ClinicManager;

namespace Project24.App.Services.P24ImageManager
{
    public class P24ImageManagerService
    {
        private class RequestData
        {
            public readonly P24IdentityUser User;
            public readonly P24ModelBase Entity;

            public RequestData(P24IdentityUser _user, P24ModelBase _entity)
            {
                User = _user;
                Entity = _entity;
            }
        }

        public class ResponseData
        {
            public bool IsSuccess { get; set; } = false;
            //public string Message { get; set; }

            public List<string> InvalidFileNames { get; set; } = new List<string>();
            public List<string> AddedFileNames { get; set; } = new List<string>();
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
        public async Task<ResponseData> UploadAsync(P24IdentityUser _user, P24ModelBase _entity, IFormFile[] _files, CancellationToken _cancellationToken = default)
        {
            m_RequestData = new RequestData(_user, _entity);
            m_ResponseData = new ResponseData();

            if (_entity is CustomerProfile)
            {
                await UploadCustomerImageAsync(_files, _cancellationToken);
                m_ResponseData.IsSuccess = true;

                return m_ResponseData;
            }

            VisitingProfile ticket = _entity as VisitingProfile;
            if (ticket != null)
            {
                await UploadTicketImageAsync(_files, _cancellationToken);
                m_ResponseData.IsSuccess = true;

                return m_ResponseData;
            }

            m_Logger.LogWarning("Something happened in UploadAsync: control reached method end.");
            return null;
        }

        /// <summary> Process image upload for CustomerImage. </summary>
        /// <param name="_files">List of files to be processed.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        private async Task UploadCustomerImageAsync(IFormFile[] _files, CancellationToken _cancellationToken)
        {
            var validFiles = ValidateFiles(_files);

            CustomerProfile customer = m_RequestData.Entity as CustomerProfile;
            List<CustomerImage> addedImages = new List<CustomerImage>(validFiles.Count);

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
            }

            await m_DbContext.AddRangeAsync(addedImages);
        }

        /// <summary> Process image upload for TicketImage. </summary>
        /// <param name="_files">List of files to be processed.</param>
        /// <param name="_cancellationToken">Cancellation Token.</param>
        private async Task UploadTicketImageAsync(IFormFile[] _files, CancellationToken _cancellationToken)
        {
            var validFiles = ValidateFiles(_files);

            VisitingProfile ticket = m_RequestData.Entity as VisitingProfile;
            CustomerProfile customer = ticket.Customer;
            List<TicketImage> addedImages = new List<TicketImage>(validFiles.Count);

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
            }

            await m_DbContext.AddRangeAsync(addedImages);
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
                _file.CopyTo(stream);
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


        private RequestData m_RequestData;
        private ResponseData m_ResponseData;

        private readonly ApplicationDbContext m_DbContext;
        private readonly ILogger<P24ImageManagerService> m_Logger;
    }

}
