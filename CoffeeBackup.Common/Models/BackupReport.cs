using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeBackup.Common.Models
{
    public class BackupReport
    {
        public BackupStatus Status { get; set; } = BackupStatus.Undefined;
        public List<string> Errors { get; set; } = new();
        public List<(string FilePath, string? Reason)> IgnoredFiles { get; set; } = new();
        public List<string> AddedFiles { get; set; } = new();
        public string ArchiveSize { get; set; } = "Undefined";
    }
}
