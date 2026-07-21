namespace Project02_TestGithub
{
    public partial class MainPage : ContentPage
    {
        // Simple in-memory models for card and tasks
        public class TaskItem : System.ComponentModel.INotifyPropertyChanged
        {
            string title;
            bool isDone;

            public string Title
            {
                get => title;
                set
                {
                    if (title == value) return;
                    title = value;
                    OnPropertyChanged();
                }
            }

            public bool IsDone
            {
                get => isDone;
                set
                {
                    if (isDone == value) return;
                    isDone = value;
                    OnPropertyChanged();
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
                => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        public class TaskCard
        {
            public string Title { get; set; }
            public System.Collections.ObjectModel.ObservableCollection<TaskItem> Tasks { get; } = new();
        }

        TaskCard currentCard;
        bool sortCompletedDescending = true;

        public MainPage()
        {
            InitializeComponent();
            currentCard = null;

            // Load saved card if exists
            LoadSavedCard();
        }

        const string SavedCardKey = "SavedTaskCard_v1";

        async void SaveCard()
        {
            try
            {
                if (currentCard == null)
                {
                    Preferences.Remove(SavedCardKey);
                    return;
                }

                var dto = new
                {
                    Title = currentCard.Title,
                    Tasks = currentCard.Tasks.Select(t => new { t.Title, t.IsDone }).ToArray()
                };

                var json = System.Text.Json.JsonSerializer.Serialize(dto);
                Preferences.Set(SavedCardKey, json);
            }
            catch
            {
                // ignore
            }
        }

        void LoadSavedCard()
        {
            try
            {
                if (!Preferences.ContainsKey(SavedCardKey))
                    return;

                var json = Preferences.Get(SavedCardKey, null);
                if (string.IsNullOrEmpty(json))
                    return;

                var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                var title = root.GetProperty("Title").GetString();
                var tasks = root.GetProperty("Tasks").EnumerateArray().Select(el => new TaskItem
                {
                    Title = el.GetProperty("Title").GetString(),
                    IsDone = el.GetProperty("IsDone").GetBoolean()
                });

                currentCard = new TaskCard { Title = title };
                foreach (var t in tasks)
                {
                    currentCard.Tasks.Add(t);
                    AttachItem(t);
                }

                CardTitleLabel.Text = currentCard.Title;
                CardContainer.IsVisible = true;
                ApplyFilterAndSort();
            }
            catch
            {
                // ignore parse errors
            }
        }

        bool appEnabled = true;

        private void OnToggleAppClicked(object sender, EventArgs e)
        {
            appEnabled = !appEnabled;
            UpdateAppEnabledState();
        }

        void UpdateAppEnabledState()
        {
            // Disable/enable main input area
            MainScroll.IsEnabled = appEnabled;
            PowerToolbarItem.Text = appEnabled ? "⏻" : "❌";
        }

        private void OnCreateCardClicked(object sender, EventArgs e)
        {
            var title = CardTitleEntry.Text?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                DisplayAlert("Lỗi", "Vui lòng nhập tên thẻ", "OK");
                return;
            }

            currentCard = new TaskCard { Title = title };
            CardTitleLabel.Text = currentCard.Title;
            TasksCollectionView.ItemsSource = currentCard.Tasks;
            CardContainer.IsVisible = true;
            CardTitleEntry.Text = string.Empty;
            SaveCard();
        }

        private void OnAddTaskClicked(object sender, EventArgs e)
        {
            if (currentCard == null)
            {
                DisplayAlert("Lỗi", "Vui lòng tạo thẻ trước", "OK");
                return;
            }

            var task = NewTaskEntry.Text?.Trim();
            if (string.IsNullOrEmpty(task))
                return;

            var newItem = new TaskItem { Title = task, IsDone = false };
            currentCard.Tasks.Add(newItem);
            AttachItem(newItem);
            NewTaskEntry.Text = string.Empty;
            ApplyFilterAndSort();
        }

        void ApplyFilterAndSort()
        {
            if (currentCard == null)
                return;

            var items = currentCard.Tasks.AsEnumerable();

            // Filter
            var fi = FilterPicker.SelectedIndex;
            if (fi == 1) // Chưa xong
                items = items.Where(t => !t.IsDone);
            else if (fi == 2) // Đã xong
                items = items.Where(t => t.IsDone);

            // Sort: completed first or last
            items = sortCompletedDescending
                ? items.OrderByDescending(t => t.IsDone).ThenBy(t => t.Title)
                : items.OrderBy(t => t.IsDone).ThenBy(t => t.Title);

            TasksCollectionView.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<TaskItem>(items);
            // Persist current card after changes
            SaveCard();
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void OnToggleSortClicked(object sender, EventArgs e)
        {
            sortCompletedDescending = !sortCompletedDescending;
            SortButton.Text = sortCompletedDescending ? "Hoàn thành xuống" : "Hoàn thành lên";
            ApplyFilterAndSort();
        }

        private void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is TaskItem item)
            {
                if (currentCard != null)
                {
                    DetachItem(item);
                    currentCard.Tasks.Remove(item);
                }
                ApplyFilterAndSort();
            }
        }

        private void OnDeleteSwipeInvoked(object sender, EventArgs e)
        {
            if (sender is SwipeItem si && si.BindingContext is TaskItem item)
            {
                if (currentCard != null)
                {
                    DetachItem(item);
                    currentCard.Tasks.Remove(item);
                }
                ApplyFilterAndSort();
            }
        }

        void AttachItem(TaskItem item)
        {
            if (item == null) return;
            item.PropertyChanged += TaskItem_PropertyChanged;
        }

        void DetachItem(TaskItem item)
        {
            if (item == null) return;
            item.PropertyChanged -= TaskItem_PropertyChanged;
        }

        private void TaskItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When IsDone changes, re-apply filter/sort and save
            if (e.PropertyName == nameof(TaskItem.IsDone))
            {
                // Ensure operation runs on UI thread
                MainThread.BeginInvokeOnMainThread(() => ApplyFilterAndSort());
            }
            else
            {
                SaveCard();
            }
        }

        // Note: CheckBox binds two-way to TaskItem.IsDone; TaskItem implements INotifyPropertyChanged
        // so UI updates per-item. No manual ItemsSource refresh required.
    }
}
