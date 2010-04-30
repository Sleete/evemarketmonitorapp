using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class AssetsProducedList : SortableCollection
    {
        public AssetsProducedList()
		{
            this.ItemType = typeof(AssetProduced);
		}

        public new AssetProduced this[int index] 
		{
			get 
			{
                return (AssetProduced)(base[index]);
			}
			set 
			{
				base[index] = value;
			}
		}

        public new AssetsProducedList GetChanges()
		{
            return (AssetsProducedList)base.GetChanges();
		}
    }
}
