using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SchoolsExample
{
    class SchoolViewModel
    {
        public SchoolViewModel()
        {
            Pupils = new ObservableCollection<PupilViewModel>();
        }

        public string Name { get; set; }
        public ObservableCollection<PupilViewModel> Pupils { get;  set; }
    }
}
