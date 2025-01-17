﻿using System;
using Android.App;
using Android.Widget;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using TEditor.Abstractions;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Android.Graphics;

namespace TEditor
{
    [Activity(Label = "TEditorActivity",
        WindowSoftInputMode = Android.Views.SoftInput.AdjustResize | Android.Views.SoftInput.StateHidden,
        Theme = "@style/Theme.AppCompat.NoActionBar.FullScreen")]
    public class TEditorActivity : Activity
    {
        const int ToolbarFixHeight = 60;
        TEditorWebView _editorWebView;
        LinearLayoutDetectsSoftKeyboard _rootLayout;
        LinearLayout _toolbarLayout;
        Toolbar _topToolBar;
        private LinearLayout toolbarItems;

        public static Action<bool, string> SetOutput { get; set; }

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //SetContentView(Resource.Layout.TEditorActivity);
            SetContentView(CreatePageForDebug());
            //_topToolBar = FindViewById<Toolbar>(Resource.Id.TopToolbar);
            _topToolBar.Title = CrossTEditor.PageTitle;
            _topToolBar.InflateMenu(Resource.Menu.TopToolbarMenu);
            _topToolBar.MenuItemClick += async (sender, e) =>
            {
                if (SetOutput != null)
                {
                    if (e.Item.TitleFormatted.ToString() == "Save")
                    {
                        string html = await _editorWebView.GetHTML();
                        SetOutput.Invoke(true, html);
                    }
                    else
                    {
                        SetOutput.Invoke(false, null);
                    }
                }
                Finish();
            };

            //_rootLayout = FindViewById<LinearLayoutDetectsSoftKeyboard>(Resource.Id.RootRelativeLayout);
            //_editorWebView = FindViewById<TEditorWebView>(Resource.Id.EditorWebView);
            //_toolbarLayout = FindViewById<LinearLayout>(Resource.Id.ToolbarLayout);

            _rootLayout.onKeyboardShown += HandleSoftKeyboardShwon;
            _editorWebView.SetOnCreateContextMenuListener(this);

            BuildToolbar();

            string htmlString = Intent.GetStringExtra("HTMLString") ?? "<p></p>";
            _editorWebView.SetHTML(htmlString);

            bool autoFocusInput = Intent.GetBooleanExtra("AutoFocusInput", false);
            _editorWebView.SetAutoFocusInput(autoFocusInput);
        }

        ViewGroup CreatePageForDebug()
        {
            _rootLayout = new LinearLayoutDetectsSoftKeyboard(this)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
            };
            _topToolBar = new Toolbar(this)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            _editorWebView = new TEditorWebView(this)
            {
                VerticalScrollBarEnabled = false,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                {
                    TopMargin=0,
                    BottomMargin=0,
                },
            };
            _editorWebView.SetBackgroundColor(Color.White);
            _toolbarLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),

            };
            _toolbarLayout.SetBackgroundColor(Color.DarkGray);
            _topToolBar.SetPadding(0, 0, 0, 0);
            var scrollToolBar = new HorizontalScrollView(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            scrollToolBar.SetPadding(0,0,0,0);
            toolbarItems = new LinearLayout(this)
            {
                Orientation=Orientation.Horizontal,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };

            _rootLayout.AddView(_topToolBar);
            _rootLayout.AddView(_editorWebView);
            _rootLayout.AddView(_toolbarLayout);
            _toolbarLayout.AddView(scrollToolBar);
            scrollToolBar.AddView(toolbarItems);
            return _rootLayout;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _rootLayout.onKeyboardShown -= HandleSoftKeyboardShwon;
        }

        public void BuildToolbar()
        {
            ToolbarBuilder builder = TEditorImplementation.ToolbarBuilder;
            if (builder == null)
                builder = new ToolbarBuilder().AddAll();

            foreach (var item in builder)
            {
                ImageButton imagebutton = new ImageButton(this);
                imagebutton.Click += (sender, e) =>
                {
                    item.ClickFunc.Invoke(_editorWebView.RichTextEditor);
                };
                string imagename = item.ImagePath.Split('.')[0];
                int resourceId = (int)typeof(Resource.Drawable).GetField(imagename).GetValue(null);
                imagebutton.SetImageResource(resourceId);
                //var toolbarItems = FindViewById<LinearLayout>(Resource.Id.ToolbarItemsLayout);
                toolbarItems.AddView(imagebutton);
            }
        }

        public void HandleSoftKeyboardShwon(bool shown, int newHeight)
        {
            if (shown)
            {
                _toolbarLayout.Visibility = Android.Views.ViewStates.Visible;
                int widthSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                int heightSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                _toolbarLayout.Measure(widthSpec, heightSpec);
                int toolbarHeight = _toolbarLayout.MeasuredHeight == 0 ? (int)(ToolbarFixHeight * Resources.DisplayMetrics.Density) : _toolbarLayout.MeasuredHeight;
                int topToolbarHeight = _topToolBar.MeasuredHeight == 0 ? (int)(ToolbarFixHeight * Resources.DisplayMetrics.Density) : _topToolBar.MeasuredHeight;
                int editorHeight = newHeight - toolbarHeight - topToolbarHeight;
                _editorWebView.LayoutParameters.Height = editorHeight;
                _editorWebView.LayoutParameters.Width = LinearLayout.LayoutParams.MatchParent;
                _editorWebView.RequestLayout();
            }
            else
            {
                if (newHeight != 0)
                {
                    _toolbarLayout.Visibility = Android.Views.ViewStates.Invisible;
                    _editorWebView.LayoutParameters = new LinearLayout.LayoutParams(-1, -1);
                    ;
                }
            }
        }

    }
}

