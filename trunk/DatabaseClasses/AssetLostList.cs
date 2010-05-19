using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class AssetLostList : SortableCollection
    {
        public AssetLostList()
		{
            this.ItemType = typeof(AssetLost);
		}

        public new AssetLost this[int index] 
		{
			get 
			{
                return (AssetLost)(base[index]);
			}
			set 
			{
				base[index] = value;
			}
		}

        public new AssetLostList GetChanges()
		{
            return (AssetLostList)base.GetChanges();
		}
    }
}
