using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskNOW
{
    struct Project
    {
        public Project(string name, int color, bool deleted, bool collapsed, int id, bool archived)
        {
            Name = name;
            Color = color;
            Deleted = deleted;
            Collapsed = collapsed;
            Id = id;
            Archived = archived;
        }

        public string Name { get; set; }
        public int Color { get; set; }
        public bool Deleted { get; set; }
        public bool Collapsed { get; set; }
        public int Id { get; set; }
        public bool Archived { get; set; }
    }
}
