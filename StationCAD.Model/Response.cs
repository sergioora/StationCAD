﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StationCAD.Model
{
    public class Response : BaseModel
    {
        public virtual OrganizationUserAffiliation UserOrg { get; set; }
        
        public virtual Incident Incident { get; set; }

        public ResponseStatus Status { get; set; }
        public DateTime StatusUpdateDateTime { get; set; }

        public TimeSpan ETA { get; set; }
        
        public string XCoordinate { get; set; }
        public string YCoordinate { get; set; }
        public string Gelocation { get; set; }
    }
    
    public enum ResponseStatus
    {
        RespondingStation = 1,
        RespondingScene,
        Available,
        Unavailable,
        Returning
    }
}
