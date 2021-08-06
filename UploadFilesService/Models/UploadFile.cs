using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadFilesService.Models
{
    public class UploadFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Finished { get; set; }
    }
}
