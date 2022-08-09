using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Collections;

namespace SchoolsExample
{
    class MainViewModel : IDropTarget
    {
        public MainViewModel()
        {
            ObservableCollection<SchoolViewModel> schools = new ObservableCollection<SchoolViewModel>();

            schools.Add(new SchoolViewModel 
            { 
                Name = "Bloomfield School",
                Pupils = new ObservableCollection<PupilViewModel>
                {
                    new PupilViewModel { FullName = "Adam James" },
                    new PupilViewModel { FullName = "Sophie Johnston" },
                    new PupilViewModel { FullName = "Kevin Sandler" },
                    new PupilViewModel { FullName = "Oscar Peterson" }
                }
            });

            schools.Add(new SchoolViewModel 
            { 
                Name = "Redacre School",
                Pupils = new ObservableCollection<PupilViewModel>
                {
                    new PupilViewModel { FullName = "Tom Jefferson" },
                    new PupilViewModel { FullName = "Tony Potts" }
                }
            });

            schools.Add(new SchoolViewModel
            {
                Name = "Top Valley School",
                Pupils = new ObservableCollection<PupilViewModel>
                {
                    new PupilViewModel { FullName = "Alex Thompson" },
                    new PupilViewModel { FullName = "Tabitha Smith" },
                    new PupilViewModel { FullName = "Carl Pederson" },
                    new PupilViewModel { FullName = "Sarah Jones" },
                    new PupilViewModel { FullName = "Paul Lowcroft" }
                }
            });

            Schools = CollectionViewSource.GetDefaultView(schools);
        }

        void IDropTarget.DragOver(DropInfo dropInfo)
        {
            if (dropInfo.Data is PupilViewModel && dropInfo.TargetItem is SchoolViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(DropInfo dropInfo)
        {
            SchoolViewModel school = (SchoolViewModel)dropInfo.TargetItem;
            PupilViewModel pupil = (PupilViewModel)dropInfo.Data;
            school.Pupils.Add(pupil);
            ((IList)dropInfo.DragInfo.SourceCollection).Remove(pupil);
        }

        public ICollectionView Schools { get; private set; }
    }
}
