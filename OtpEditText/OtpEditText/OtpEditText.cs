using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;
using System;
using static Android.Graphics.Paint;

namespace OtpEditText
{
    public class OtpEditText : AppCompatEditText
    {

        public static readonly string XML_NAMESPACE_ANDROID = "http://schemas.android.com/apk/res/android";
        private int defStyleAttr = 0;

        private float mSpace = 8; //24 dp by default, space between the lines
        private float mCharSize;
        private float mNumChars = 6;
        private float mLineSpacing = 10; //8dp by default, height of the text from our lines
        private int mMaxLength = 6;

        private IOnClickListener mClickListener;

        private float mLineStroke = 1; //1dp by default
        private float mLineStrokeSelected = 2; //2dp by default
        private Paint mLinesPaint;

        private int mMainColor;
        private int mSecondaryColor;
        private int mTextColor;

        private Paint mStrokePaint;

        public OtpEditText(Context context) : base(context) { }

        public OtpEditText(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init(context, attrs);
        }

        public OtpEditText(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            this.defStyleAttr = defStyleAttr;
            init(context, attrs);
        }

        protected OtpEditText(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        private void init(Context context, IAttributeSet attrs)
        {
            getAttrsFromTypedArray(attrs);

            float multi = context.Resources.DisplayMetrics.Density;
            mLineStroke = multi * mLineStroke;
            mLineStrokeSelected = multi * mLineStrokeSelected;
            mLinesPaint = new Paint(this.Paint);
            mStrokePaint = new Paint(this.Paint);
            mStrokePaint.StrokeWidth = 4;
            mStrokePaint.SetStyle(Style.Stroke);
            mLinesPaint.StrokeWidth = mLineStroke;
            SetBackgroundResource(0);
            mSpace = multi * mSpace; //convert to pixels for our density
            mNumChars = mMaxLength;

            base.CustomSelectionActionModeCallback = new Callback();
            base.Click += (sender, e) =>
            {
                SetSelection(this.Text.Length);
                if (mClickListener != null)
                {
                    mClickListener.OnClick(this);
                }
            };
        }

        private void getAttrsFromTypedArray(IAttributeSet attributeSet)
        {
            TypedArray a = this.Context.ObtainStyledAttributes(attributeSet, Resource.Styleable.OtpEditText, defStyleAttr, 0);

            mMaxLength = attributeSet.GetAttributeIntValue(XML_NAMESPACE_ANDROID, "maxLength", 4);
            mMainColor = a.GetColor(Resource.Styleable.OtpEditText_oev_primary_color, Android.Resource.Color.HoloRedDark);
            mSecondaryColor = a.GetColor(Resource.Styleable.OtpEditText_oev_secondary_color, Resource.Color.light_gray);
            mTextColor = a.GetColor(Resource.Styleable.OtpEditText_oev_text_color, Android.Resource.Color.Black);

            a.Recycle();
        }

        public override ActionMode.ICallback CustomSelectionActionModeCallback { get => base.CustomSelectionActionModeCallback; set => base.CustomSelectionActionModeCallback = value; }

        protected override void OnDraw(Canvas canvas)
        {
            int availableWidth = this.Width - this.PaddingRight - this.PaddingLeft;
            if (mSpace < 0)
            {
                mCharSize = (availableWidth / (mNumChars * 2 - 1));
            }
            else
            {
                mCharSize = (availableWidth - (mSpace * (mNumChars - 1))) / mNumChars;
            }

            mLineSpacing = (float)(this.Height * .6);

            int startX = this.PaddingLeft;
            int bottom = this.Height - this.PaddingBottom;
            int top = this.PaddingTop;

            //Text Width
            string text = this.Text;
            int textLength = text.Length;
            float[] textWidths = new float[textLength];
            this.Paint.GetTextWidths(this.Text, 0, textLength, textWidths);

            for (int i = 0; i < mNumChars; i++)
            {
                updateColorForLines(i <= textLength, i == textLength, this.Text.Length, (int)mNumChars);
                canvas.DrawLine(startX, bottom, startX + mCharSize, bottom, mLinesPaint);

                try
                {
                    canvas.DrawRoundRect(startX, top, startX + mCharSize, bottom, 8, 8, mLinesPaint);
                    canvas.DrawRoundRect(startX, top, startX + mCharSize, bottom, 8, 8, mStrokePaint);
                }
                catch (NoSuchMethodError)
                {
                    canvas.DrawRect(startX, top, startX + mCharSize, bottom, mLinesPaint);
                    canvas.DrawRect(startX, top, startX + mCharSize, bottom, mStrokePaint);
                }
                if (this.Text.Length > i)
                {
                    float middle = startX + mCharSize / 2;
                    canvas.DrawText(text, i, i + 1, middle - textWidths[0] / 2, mLineSpacing, this.Paint);
                }

                if (mSpace < 0)
                {
                    startX += (int)(mCharSize * 2);
                }
                else
                {
                    startX += (int)(mCharSize + mSpace);
                }
            }
        }

        private void updateColorForLines(bool next, bool current, int textSize, int totalSize)
        {
            if (next)
            {
                mStrokePaint.Color = new Color(mSecondaryColor);
                mLinesPaint.Color = new Color(mSecondaryColor);
            }
            else
            {
                mStrokePaint.Color = new Color(mSecondaryColor);
                mLinesPaint.Color = new Color(ContextCompat.GetColor(this.Context, Android.Resource.Color.White));
            }
            if (current)
            {
                mLinesPaint.Color = new Color(ContextCompat.GetColor(this.Context, Android.Resource.Color.White));
                mStrokePaint.Color = new Color(mMainColor);
            }
        }

        public override void SetOnClickListener(IOnClickListener l)
        {
            mClickListener = l;
        }

        public override ActionMode.ICallback CustomInsertionActionModeCallback { get => base.CustomInsertionActionModeCallback; set => throw new RuntimeException("setCustomSelectionActionModeCallback() not supported."); }
    }

    class Callback : Java.Lang.Object, ActionMode.ICallback
    {
        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            return false;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        public void OnDestroyActionMode(ActionMode mode) { }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }
    }
}