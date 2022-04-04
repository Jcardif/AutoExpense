using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AutoExpense.Android.Interfaces;

namespace AutoExpense.Android.Adapters
{
    public class SendersAdapter : RecyclerView.Adapter
    {
        public ISendersManager SendersManager { get; }
        private readonly List<string> _senders;
        private readonly List<string> _selectSenders;

        public SendersAdapter(List<string> senders, List<string> selectSenders, ISendersManager sendersManager)
        {
            SendersManager = sendersManager;
            _senders = senders;
            _selectSenders = selectSenders;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is SendersViewHolder vh)
            {
                vh.SendersCheckBox.Text = _senders[position];
                if (_selectSenders.Contains(_senders[position]))
                    vh.SendersCheckBox.Checked = true;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.item_sender, parent, false);
            var checkbox = view?.FindViewById<CheckBox>(Resource.Id.sender_checkbox);

            checkbox.CheckedChange += (s, e) =>
            {
                if (e.IsChecked && !_selectSenders.Contains(checkbox.Text))
                    SendersManager.SenderSelected(checkbox.Text);
                else
                {
                    SendersManager.SenderRemoved(checkbox.Text);
                }
            };

            var holder = new SendersViewHolder(view, checkbox);
            return holder;
        }

        public override int ItemCount => _senders.Count;
    }

    public class SendersViewHolder : RecyclerView.ViewHolder
    {
        public View ItemView { get; }
        public CheckBox SendersCheckBox { get; }

        public SendersViewHolder(View itemView, CheckBox sendersCheckBox) : base(itemView)
        {
            ItemView = itemView;
            SendersCheckBox = sendersCheckBox;
        }
    }
}