using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fort.Utils.Logger;

namespace Fort.Database.Entities
{
    public class Log
    {
        public int Id { get; set; }

        [StringLength(30)]
        public string Level { get; set; }
        [StringLength(10)]
        public string Player { get; set; }
        [StringLength(200)]
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime At { get; set; }

        [NotMapped]
        public ELogLevel LevelE
        {
            get
            {
                foreach (ELogLevel l in Enum.GetValues(typeof(ELogLevel)))
                    if (l.ToString() == Level)
                        return l;

                throw new InvalidOperationException("Invalid log level");
            }
            set
            {
                Level = value.ToString();
            }
        }
    }
}