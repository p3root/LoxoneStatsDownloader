using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LoxoneStatisticLoader.Annotations;

namespace LoxoneStatisticLoader
{
    public class RootStatstic : INotifyPropertyChanged
    {
        public List<Statistic> Files { get; set; } = new List<Statistic>();
        private bool? _isChecked = true;
        public bool? IsChecked
        {
            get => _isChecked;
            set => SetIsChecked(value, true);
        }

        public void SetIsChecked(bool? value, bool updateChildren)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (var file in Files)
                {
                    file.SetIsChecked(_isChecked, false);
                }
            }

            OnPropertyChanged(nameof(IsChecked));
        }

        public void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < Files.Count; ++i)
            {
                bool? current = Files[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class Statistic: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Guid { get; set; }

        public ObservableCollection<StatisticFile> Files { get; set; } = new ObservableCollection<StatisticFile>();

        private bool? _isChecked = true;
        public bool? IsChecked
        {
            get => _isChecked;
            set => this.SetIsChecked(value, true);
        }

        public void SetIsChecked(bool? value, bool updateChildren)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (var file in Files)
                {
                    file.SetIsChecked(_isChecked, false, true);
                }
            }

            this.OnPropertyChanged(nameof(IsChecked));
        }

        public void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < Files.Count; ++i)
            {
                bool? current = Files[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StatisticFile : INotifyPropertyChanged
    {
        private readonly Statistic _parent;
        public string Name { get; set; }
        public string Url { get; set; }

        private bool? _isChecked = true;
        public bool? IsChecked
        {
            get => _isChecked;
            set => this.SetIsChecked(value, true, true);
        }

        public StatisticFile(Statistic parent)
        {
            _parent = parent;
        }

        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            OnPropertyChanged(nameof(IsChecked));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
