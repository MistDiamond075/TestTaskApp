using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskApp.utils
{
    public class FilePickerRequests
    {
        public record OpenFileRequest(string Title, string[] Extensions);
        public record SaveFileRequest(string Title, string SuggestedFileName, string[] Extensions);
    }
}
