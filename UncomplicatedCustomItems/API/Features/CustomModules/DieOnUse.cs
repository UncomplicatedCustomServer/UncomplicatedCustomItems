using UncomplicatedCustomItems.Enums;

namespace UncomplicatedCustomItems.API.Features.CustomModules
{
    public class DieOnUse : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.DieOnUse;
    }
}