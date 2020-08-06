using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGS.Helper
{
    public class MusicHelper
    {
        public class Album
        {
            public string Title { get; set; }
            public string Version { get; set; }

            public string ID { get; set; }
            public string UID { get; set; }
            public int Duration { get; set; }
            public int NumberOfTracks { get; set; }
            public int NumberOfVideos { get; set; }
            public int NumberOfVolumes { get; set; }
            public string Cover { get; set; }

            public string Url { get; set; }
            public bool Explicit { get; set; }

            public string ReleaseDate { get; set; }

            public Artist Artist { get; set; }
            public ObservableCollection<Artist> Artists { get; set; }
            public ObservableCollection<Track> Tracks { get; set; }
            public ObservableCollection<Video> Videos { get; set; }

            public string DurationStr { get { return TimeHelper.ConverIntToString(Duration); } }
            public string ArtistsName { get { return GetArtists(Artists); } }
        }

        public class Artist
        {
            public string ID { get; set; }
            public string UID { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string Cover { get; set; }
        }

        public class Track 
        {
            public string Title { get; set; }
            public string TitleBrief { get; set; }
            public bool   Live { get; set; }
            public string Version { get; set; }

            public string ID { get; set; }
            public string UID { get; set; }
            public int    Duration { get; set; }

            public string Url { get; set; }
            public bool   Explicit { get; set; }

            public int    TrackNumber { get; set; }
            public int    VolumeNumber { get; set; }
            public Album  Album { get; set; }

            public Artist Artist { get; set; }
            public ObservableCollection<Artist> Artists { get; set; }

            public string DurationStr { get { return TimeHelper.ConverIntToString(Duration); } }
            public string ArtistsName { get { return GetArtists(Artists); } }
        }

        public class Video
        {
            public string Title { get; set; }
            public string Version { get; set; }

            public string ID { get; set; }
            public string UID { get; set; }
            public int    Duration { get; set; }

            public string Url { get; set; }
            public bool   Explicit { get; set; }
            public string Cover { get; set; }

            public int    TrackNumber { get; set; }
            public string ReleaseDate { get; set; }
            public Album  Album { get; set; }

            public Artist Artist { get; set; }
            public ObservableCollection<Artist> Artists { get; set; }

            public string DurationStr { get { return TimeHelper.ConverIntToString(Duration); } }
            public string ArtistsName { get { return GetArtists(Artists); } }
        }

        



        static string[] GetArtistsList(ObservableCollection<Artist> Artists)
        {
            if (Artists == null)
                return null;
            List<string> names = new List<string>();
            foreach (var item in Artists)
                names.Add(item.Name);
            return names.ToArray();
        }

        static string GetArtists(ObservableCollection<Artist> Artists)
        {
            if (Artists == null)
                return null;
            string[] names = GetArtistsList(Artists);
            string ret = string.Join(" / ", names);
            return ret;
        }

    }
}
