using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ProtocolManagement.ViewModels.Models
{
    public class PrepareExpandNode
    {
        public string ProtocolTemplateId { get; set; } = string.Empty;

        public Node SelectedNode { get; set; } = new Node();
    }
}
