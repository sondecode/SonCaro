﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SonCaro.Models
{
    [Table("Matches")]
    public class Match
    {
        [Key]
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }

        public Guid FirstUserId { get; set; }
        public Guid SecondUserId { get; set; }
        public Guid CreatedRoomUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public int FirstScore { get; set; }

        public int SecondScore { get; set; }

        public Guid WinnerId { get; set; }

        public List<MatchDetail> MatchDetails { get; set; }
    }
}
