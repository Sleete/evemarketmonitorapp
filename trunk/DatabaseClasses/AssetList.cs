using System;
using System.Collections.Generic;
using System.Text;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class AssetList : SortableCollection
    {
        public AssetList()
		{
			this.ItemType = typeof(Asset);
		}

        public new Asset this[int index] 
		{
			get 
			{
                return (Asset)(base[index]);
			}
			set 
			{
				base[index] = value;
			}
		}

        public new AssetList GetChanges()
		{
            return (AssetList)base.GetChanges();
		}

        public void Add(AssetList list)
        {
            foreach(Asset asset in list) 
            {
                this.Add(asset);
            }
        }
    }
}
