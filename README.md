using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MusicPlaylistManager
{

    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        // duration stored as TimeSpan to make comparisons easier for me (HH:mm:ss or mm:ss)
        public TimeSpan Duration { get; set; }
        public string Genre { get; set; }

        public Song(string title, string artist, string album, TimeSpan duration, string genre)
        {
            Title = title;
            Artist = artist;
            Album = album;
            Duration = duration;
            Genre = genre;
        }

        public override string ToString()
        {
            //output for console display
            return $"{Title} — {Artist} ({Album}) [{Duration:hh\\:mm\\:ss}] Genre: {Genre}";
        }
    }

    class Node
    {
        public Song Data;
        public Node Prev;
        public Node Next;

        public Node(Song s)
        {
            Data = s;
        }
    }


    public class DoublyLinkedPlaylist
    {
        private Node head;
        private Node tail;
        public int Count { get; private set; }

        // cursor for playback
        private Node current;

        public DoublyLinkedPlaylist()
        {
            head = tail = null;
            Count = 0;
            current = null;
        }

        // append song to the end
        public void AddSong(Song s)
        {
            var node = new Node(s);
            if (head == null)
            {
                head = tail = node;
            }
            else
            {
                tail.Next = node;
                node.Prev = tail;
                tail = node;
            }
            Count++;
        }

        // insert at position 
        public void AddSongAt(Song s, int pos)
        {
            if (pos <= 0)
            {
                var node = new Node(s);
                if (head == null)
                {
                    head = tail = node;
                }
                else
                {
                    node.Next = head;
                    head.Prev = node;
                    head = node;
                }
                Count++;
                return;
            }

            if (pos >= Count)
            {
                AddSong(s);
                return;
            }

            var cur = head;
            for (int i = 0; i < pos - 1; i++) cur = cur.Next;
            var newNode = new Node(s);
            newNode.Next = cur.Next;
            newNode.Prev = cur;
            cur.Next.Prev = newNode;
            cur.Next = newNode;
            Count++;
        }

        // remove first song with matching title 
        public bool RemoveByTitle(string title)
        {
            var cur = head;
            while (cur != null)
            {
                if (string.Equals(cur.Data.Title, title, StringComparison.OrdinalIgnoreCase))
                {
                    RemoveNode(cur);
                    return true;
                }
                cur = cur.Next;
            }
            return false;
        }

        // remove by position
        public bool RemoveByPosition(int pos)
        {
            if (pos < 0 || pos >= Count) return false;
            var cur = head;
            for (int i = 0; i < pos; i++) cur = cur.Next;
            RemoveNode(cur);
            return true;
        }

        // internal helper to unlink a node
        private void RemoveNode(Node n)
        {
            if (n.Prev != null) n.Prev.Next = n.Next; else head = n.Next;
            if (n.Next != null) n.Next.Prev = n.Prev; else tail = n.Prev;
            if (current == n) current = n.Next ?? n.Prev; // move cursor if it pointed to removed node
            Count--;
        }

        // Find songs by artist
        public List<Song> FindByArtist(string artist)
        {
            var list = new List<Song>();
            var cur = head;
            while (cur != null)
            {
                if (string.Equals(cur.Data.Artist, artist, StringComparison.OrdinalIgnoreCase))
                    list.Add(cur.Data);
                cur = cur.Next;
            }
            return list;
        }

        // find song by title 
        public int FindByTitle(string title)
        {
            int index = 0;
            var cur = head;
            while (cur != null)
            {
                if (string.Equals(cur.Data.Title, title, StringComparison.OrdinalIgnoreCase))
                    return index;
                index++;
                cur = cur.Next;
            }
            return -1;
        }

        // display all songs in playlist
        public void DisplayAll()
        {
            Console.WriteLine($"Playlist — {Count} song(s):");
            int idx = 0;
            var cur = head;
            while (cur != null)
            {
                Console.WriteLine($"{idx++:D2}: {cur.Data}");
                cur = cur.Next;
            }
        }

        // display songs by duration range 
        public void DisplayByDuration(TimeSpan min, TimeSpan max)
        {
            Console.WriteLine($"Songs between {min} and {max}:");
            var cur = head;
            int idx = 0;
            while (cur != null)
            {
                if (cur.Data.Duration >= min && cur.Data.Duration <= max)
                    Console.WriteLine($"{idx}: {cur.Data}");
                idx++; cur = cur.Next;
            }
        }

        // playback methods
        public void StartPlayingFrom(int pos = 0)
        {
            if (Count == 0)
            {
                Console.WriteLine("Playlist is empty.");
                return;
            }
            if (pos < 0) pos = 0;
            if (pos >= Count) pos = Count - 1;
            current = head;
            for (int i = 0; i < pos; i++) current = current.Next;
            PlayCurrent();
        }

        // play current song (simulate a small wait cos it seems like loading)
        public void PlayCurrent()
        {
            if (current == null)
            {
                Console.WriteLine("No song selected.");
                return;
            }
            Console.WriteLine($"\nNow playing: {current.Data}");
            Thread.Sleep(400);
        }

        // move to next song and play
        public void PlayNext()
        {
            if (current == null) { StartPlayingFrom(0); return; }
            if (current.Next != null) current = current.Next;
            else
            {
                Console.WriteLine("Reached end of playlist; staying on last song.");
            }
            PlayCurrent();
        }

        //move to previous song and play
        public void PlayPrevious()
        {
            if (current == null) { StartPlayingFrom(0); return; }
            if (current.Prev != null) current = current.Prev;
            else
            {
                Console.WriteLine("At start of playlist; staying on first song.");
            }
            PlayCurrent();
        }

        // shuffle: convert to list, shuffle with Fisher-Yates, and then reconstruct linked list
        public void Shuffle()
        {
            if (Count <= 1) return;
            var arr = new List<Song>(Count);
            var cur = head;
            while (cur != null) { arr.Add(cur.Data); cur = cur.Next; }

            var rnd = new Random();
            for (int i = arr.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
            }

            // rebuild list
            head = tail = null;
            Count = 0;
            foreach (var s in arr) AddSong(s);

            // reset playback to start
            current = head;
        }

        // convert playlist to list of songs
        public List<Song> ToList()
        {
            var result = new List<Song>();
            var cur = head;
            while (cur != null)
            {
                result.Add(cur.Data);
                cur = cur.Next;
            }
            return result;
        }
    }

    class Program
    {
        // helper to parse durations
  
        static TimeSpan ParseDurationFromTimeOfDay(string timeStr)
        {
            // ry to parse "hh:mm:ss tt" and then return its TimeOfDay
            if (DateTime.TryParse(timeStr, out var dt))
            {
                return dt.TimeOfDay;
            }
            //fallback: try mm:ss or hh:mm:ss
            if (TimeSpan.TryParse(timeStr, out var ts)) return ts;
            //default if unparsable
            return TimeSpan.Zero;
        }

        static void Main()
        {
            var playlist = new DoublyLinkedPlaylist();

            // the songs
            var rawSongs = new List<(string Title, string Artist, string Album, string TimeStr, string Genre)>()
            {
                ("Shape of You","Ed Sheeran","Divide","03:53:00 AM","Pop"),
                ("Bohemian Rhapsody","Queen","A Night at the Opera","05:55:00 AM","Rock"),
                ("Blinding Lights","The Weeknd","After Hours","03:20:00 AM","Synth-Pop"),
                ("Rolling in the Deep","Adele","21","03:48:00 AM","Soul"),
                ("Hotel California","Eagles","Hotel California","06:31:00 AM","Rock"),
                ("Perfect","Ed Sheeran","Divide","04:23:00 AM","Pop"),
                ("Levitating","Dua Lipa","Future Nostalgia","03:23:00 AM","Disco-Pop"),
                ("Uptown Funk","Mark Ronson ft. Bruno Mars","Uptown Special","04:30:00 AM","Funk"),
                ("Bad Guy","Billie Eilish","When We All Fall Asleep","03:14:00 AM","Electropop"),
                ("Counting Stars","OneRepublic","Native","04:17:00 AM","Pop-Rock")
            };

            foreach (var r in rawSongs)
            {
                var dur = ParseDurationFromTimeOfDay(r.TimeStr); 
                playlist.AddSong(new Song(r.Title, r.Artist, r.Album, dur, r.Genre));
            }

            // demonstraation
            Console.WriteLine(">>> Initial playlist loaded:");
            playlist.DisplayAll();

            //search
            Console.WriteLine("\n>>> Searching for 'Perfect':");
            int pos = playlist.FindByTitle("Perfect");
            Console.WriteLine(pos >= 0 ? $"Found at position {pos}" : "Not found");

            //display based on artist
            Console.WriteLine("\n>>> Songs by Ed Sheeran:");
            var edSongs = playlist.FindByArtist("Ed Sheeran");
            foreach (var s in edSongs) Console.WriteLine(s);

            //remove song by title
            Console.WriteLine("\n>>> Removing 'Bad Guy' by title...");
            bool removed = playlist.RemoveByTitle("Bad Guy");
            Console.WriteLine(removed ? "Removed." : "Song not found.");
            playlist.DisplayAll();

            //remove song at position
            Console.WriteLine("\n>>> Removing song at position 2...");
            bool removedPos = playlist.RemoveByPosition(2);
            Console.WriteLine(removedPos ? "Removed." : "Position out of range.");
            playlist.DisplayAll();

            //add new song at position
            Console.WriteLine("\n>>> Adding a new song at position 1...");
            var newSong = new Song("New song", "Artist", "Album", TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(10)), "Demo");
            playlist.AddSongAt(newSong, 1);
            playlist.DisplayAll();

            // shuffle
            Console.WriteLine("\n>>> Shuffling playlist...");
            playlist.Shuffle();
            playlist.DisplayAll();

            //playback example
            Console.WriteLine("\n>>> Playback demo starting from first song:");
            playlist.StartPlayingFrom(0);
            playlist.PlayNext();
            playlist.PlayNext();
            playlist.PlayPrevious();

            // display songs within duration range
            Console.WriteLine("\n>>> Songs between 03:15:00 and 04:00:00:");
            playlist.DisplayByDuration(TimeSpan.Parse("03:15:00"), TimeSpan.Parse("04:00:00"));

            //final state
            Console.WriteLine("\n>>> Final playlist snapshot:");
            playlist.DisplayAll();

            Console.WriteLine("\nDemo complete. Press any key to exit.");
            //Console.ReadKey();
        }
    }
}
