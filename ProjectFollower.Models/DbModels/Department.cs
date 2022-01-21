﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProjectFollower.Models.DbModels
{
    public class Department
    {
        [Key]
        //Lütfen Personelinizin pozisyonunu seçiniz..
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
