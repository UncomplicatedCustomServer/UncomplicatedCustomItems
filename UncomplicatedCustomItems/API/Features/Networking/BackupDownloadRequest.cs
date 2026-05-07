using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomItems.API.Features.Networking
{
    public class BackupDownloadRequest : APIRequest
    {
        public override string Endpoint => "backup/download";

        public override RequestType Type => RequestType.Get;

        
    }
}
