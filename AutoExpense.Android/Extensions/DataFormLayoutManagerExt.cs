﻿using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Syncfusion.Android.DataForm;

namespace AutoExpense.Android.Extensions
{
    public class DataFormLayoutManagerExt : DataFormLayoutManager
    {

        private EditText _editText;
        private static EditText _myEdtTxt;


        public DataFormLayoutManagerExt(SfDataForm dataForm) : base(dataForm)
        {
        }

        protected override View GenerateViewForLabel(DataFormItem dataFormItem)
        {
            var label = base.GenerateViewForLabel(dataFormItem);
            if (label is TextView view)
            {
                view.Typeface = Typeface.Default;
                view.TextSize = 16;
                view.SetTextColor(Color.White);
            }
            return label;
        }



        protected override void OnEditorCreated(DataFormItem dataFormItem, View editor)
        {
            if (editor is EditText edtTxt)
            {
                _editText = edtTxt;
            }


            _editText.Typeface = Typeface.Default;
            _editText.SetBackgroundResource(Resource.Drawable.syncfusion_edittext_style);
            _editText.SetTextColor(Color.White);
            _editText.SetHintTextColor(Color.WhiteSmoke);
            _editText.InputType = InputTypes.TextFlagMultiLine;



        }

    }
}