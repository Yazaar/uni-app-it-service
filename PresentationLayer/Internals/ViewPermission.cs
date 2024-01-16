using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationLayer.Internals
{
    public class ViewPermission
    {
        public string Visibility { get => HasPermission ? "Visible" : "Collapsed"; }
        
        public bool HasPermission { get; }

        public ViewPermission(Roll? currentRole, IEnumerable<string> permissionRoleAuthIDs)
        {
            if (currentRole is null) HasPermission = false;
            else HasPermission = permissionRoleAuthIDs.Contains(currentRole.RollBehörighet.RollBehörighetID);
        }

    }
}
