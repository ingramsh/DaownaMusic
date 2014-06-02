﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaownaMp3Library
{
    public class PlayList
    {

        [Key]
        public int PlaylistID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public Member Member
        {
            get;
            set;
        }
        public bool IsPublic
        {
            get;
            set;
        }
        public List<Track> Tracks
        {
            get;
            set;
        }
        public List<int> PlayListOrder
        {
            get
            {
                if (_playlistOrder == null)
                    _playlistOrder = DataAccess.Instance.GetPlayListTrackOrder(_playlistId);

                return _playlistOrder;
            }
        }
        public void PlayListSyncOrder()
        {
            List<Track> holder = new List<Track>();
            _orderedTracks = new List<Track>();
            _playlistTrackIds = new List<int>();
            Track hold = new Track(0, 0, 0);
            int point = 0;

            if (_playlistOrder == null)
                _playlistOrder = this.PlayListOrder;

            //inefficient sort loop series for ordering _playlistTrackIds & _orderedTracks
            for (int i = 0; i < (_playlistOrder.Count) / 3; i++)
            {
                if ((point == 0 && _playlistOrder[(i * 3) + 2] < 0) || (point == _playlistOrder[(i * 3) + 2]))
                {
                    _orderedTracks.Add(new Track(_playlistOrder[i * 3], _playlistOrder[(i * 3) + 1], _playlistOrder[(i * 3) + 2]));
                    point = _playlistOrder[i * 3];
                    _playlistTrackIds.Add(_playlistOrder[(i * 3) + 1]); 
                }
                else 
                    holder.Add(new Track(_playlistOrder[i*3], _playlistOrder[(i*3) + 1], _playlistOrder[(i*3) + 2]));
            }
            while (_orderedTracks.Count < (_playlistOrder.Count) / 3)
            {
                hold = holder.Find(x => x.OrderPoint == point);
                _orderedTracks.Add(hold);
                point = hold.PlaylistTrackId;
                _playlistTrackIds.Add(_orderedTracks.Last().TrackId);
                holder.Remove(hold);
            }
        }
        public void RemoveTrack(int trackId)
        {
            int ahead, behind;

            PlayListSyncOrder();
            Track replace = _orderedTracks.Find(x => x.TrackId == trackId);
            ahead = replace.OrderPoint;
            behind = replace.PointedBy;

            if (behind > -1)
                DataAccess.Instance.PlayListTrackOrderChange(behind, ahead);
            if (DataAccess.Instance.RemovePlayListTracks(_playlistId, trackId))
            {
                _playlistTrackIds = null;
            }
            else
            {
                //no track deleted from playlist
            }

            _playlistOrder = null;
        }
        public bool AddTrack(int trackId)
        {
            PlayListSyncOrder();
            int order;
            
            if (_playlistTrackIds.Count == 0)
                order = -_playlistId;
            else
                order = _orderedTracks.Last().PlaylistTrackId;
            
            if (DataAccess.Instance.FindPlayListTrackId(_playlistId, trackId))
                //identical PlayListTrack found
                return false;
            else
            {
                DataAccess.Instance.AddPlayListTrack(_playlistId, trackId, order);
                _playlistTrackIds = null;
                return true;
            }
        }
        public void SwapTrackOrder(int upTrackId, int downTrackId)
        {
            int upPlaylistTrackId = _orderedTracks.Find(x => x.TrackId == upTrackId).PlaylistTrackId;
            int downPlaylistTrackId = _orderedTracks.Find(x => x.TrackId == upTrackId).OrderPoint;
            int frontNeighbor = DataAccess.Instance.GetPlayListTrackOrderPoint(downPlaylistTrackId);
            int backNeighbor = DataAccess.Instance.GetPlayListTrackIdPointer(upPlaylistTrackId);

            DataAccess.Instance.PlayListTrackOrderChange(upPlaylistTrackId, frontNeighbor);
            DataAccess.Instance.PlayListTrackOrderChange(backNeighbor, downPlaylistTrackId);
            DataAccess.Instance.PlayListTrackOrderChange(downPlaylistTrackId, upPlaylistTrackId);

            _playlistTrackIds = null;
            _playlistOrder = null;
        }
    }
}
