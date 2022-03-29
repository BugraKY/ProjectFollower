﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectFollower.Models.DbModels
{
    public class Notifications
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool Readed { get; set; }
    }
}
