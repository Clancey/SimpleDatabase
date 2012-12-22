using System;
using Xamarin.Tables;
#if iOS
using MonoTouch.UIKit;
#elif Android
using Android.Widget;
using Android.Content;
#endif
namespace InstantSample
{
	public class SongViewModel : TableViewModel<Song>
	{
		#if iOS
		public SongViewModel ()
		{
		}
		#elif Android
		public SongViewModel(Context context, ListView listView,int sectionedListSeparatorLayout = Android.Resource.Layout.SimpleListItem1) : base(context,listView,sectionedListSeparatorLayout )
		{

		}
		
		#endif
		#region implemented abstract members of TableViewModel

		public override int RowsInSection (int section)
		{
			return SongDatabase.Main.RowsInSection<Song> (section);
		}

		public override ICell GetICell (int section, int position)
		{
			var song =  SongDatabase.Main.ObjectForRow<Song> (section,position);
			return new StringCell (song.Title);
		}

		public override int NumberOfSections ()
		{
			return SongDatabase.Main.NumberOfSections<Song> ();
		}

		public override int GetItemViewType (int section, int row)
		{
			throw new NotImplementedException ();
		}

		public override string[] SectionIndexTitles ()
		{
			return SongDatabase.Main.QuickJump<Song> ();
		}

		public override string HeaderForSection (int section)
		{
			return SongDatabase.Main.SectionHeader<Song> (section);
		}

		public override void RowSelected (Song item)
		{

		}

		public override Song ItemFor (int section, int row)
		{
			
			return SongDatabase.Main.ObjectForRow<Song> (section,row);
		}

		#endregion
	}
}

