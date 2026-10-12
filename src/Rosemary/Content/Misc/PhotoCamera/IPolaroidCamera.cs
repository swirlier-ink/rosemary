using System;
using System.Collections.Generic;
using System.Text;
using Terraria;

namespace Rosemary.Content.Misc;

public interface IPolaroidCamera
{
    List<Item> Filters { set; get; }
}
