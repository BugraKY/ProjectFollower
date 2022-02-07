﻿using ProjectFollower.Models.DbModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectFollower.Models.ViewModels
{
    public class ProjectDetailVM
    {
        [ForeignKey("ProjectsId")]
        public Guid ProjectsId { get; set; }
        public Projects Project { get; set; }
        public IEnumerable<ProjectTasks> ProjectTasks { get; set; }
        public IEnumerable<ProjectDocuments> ProjectDocuments { get; set; }
        public IEnumerable<CommentVM> CommentVM { get; set; }
        public IEnumerable<Users> Users { get; set; }
        public ProjectTasks ProjectTaskItem { get; set; }
    }
}
