using System;
using Android.Views;

namespace AutoExpense.Android.Interfaces
{
	public interface IItemClickListener
	{
		void OnItemClick(View itemView, int position, bool isLongClick);
	}
}

