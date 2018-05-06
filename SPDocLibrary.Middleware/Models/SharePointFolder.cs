using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPDocLibrary.Middleware.Models
{
    public class SharePointFolder
    {
        public SharePointFolder(string folderName, string folderPath, int totalFiles, DateTime createdOn)
        {
            FolderName = folderName;
            FolderPath = folderPath;
            TotalFiles = totalFiles;
            CreatedOn = createdOn;
        }

        public string FolderName { get; set; }
        public string FolderPath { get; set; }
        public int TotalFiles { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}