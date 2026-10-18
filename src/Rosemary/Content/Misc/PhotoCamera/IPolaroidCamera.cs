using System.Collections.Generic;
using Terraria;

namespace Rosemary.Content.Misc;

public interface IPolaroidCamera
{
    List<Item> Filters { set; get; }
}
