using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Todolist.API;
using Todolist.DataModel;
using System.Diagnostics;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ToDoList
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>

    public sealed partial class MainPage : Page
    {
        private OneNote OneNoteAPI;
        public Tasklist tasklist { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.Ajouter.IsEnabled = false;
            this.AutoSuggestBox.IsEnabled = false;
            this.Supprimer.IsEnabled = false;
            this.InitTasklist();
        }

        private async void InitTasklist()
        {
            try
            {
                this.ProgressRing.IsActive = true;
                this.OneNoteAPI = new OneNote();
                this.OneNoteAPI.Init();
                tasklist = await this.OneNoteAPI.GetNotes();
                Debug.WriteLine(tasklist.Count);
            }
            finally
            {
                TodoList.ItemsSource = tasklist;
                ProgressRing.IsActive = false;
                this.Ajouter.IsEnabled = this.Supprimer.IsEnabled = this.AutoSuggestBox.IsEnabled = true;
            }
            
        }

        private async void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            AddTodoDialog dialog = new AddTodoDialog();
            await dialog.ShowAsync();

            if (dialog.NewTodo != null)
            {
                try
                {
                    this.ProgressRing.IsActive = true;
                    dialog.NewTodo.Url = await this.OneNoteAPI.CreateNote(dialog.NewTodo.Serialize());
                }
                finally
                {
                    this.tasklist.Insert(0, dialog.NewTodo);
                    this.ProgressRing.IsActive = false;
                    TodoList.ItemsSource = tasklist;
                }
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string userInput = sender.Text;
                TodoList.ItemsSource = tasklist.MatchTodos(userInput).ToList();
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item, take an action on it here
            }
            else
            {
                string userInput = sender.Text;
                
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private async void TodoList_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.ProgressRing.IsActive = true;
            ToDo selected = e.ClickedItem as ToDo;
            int idx = this.tasklist.IndexOf(selected);
            EditTodoDialog editDialog = new EditTodoDialog(selected, this.OneNoteAPI);
            await editDialog.ShowAsync();
            this.tasklist[idx] = editDialog.task;
            this.ProgressRing.IsActive = false;
        }

        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            this.TodoList.IsItemClickEnabled = !this.TodoList.IsItemClickEnabled;
            this.TodoList.SelectionMode = (this.TodoList.IsItemClickEnabled == false) ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            this.Valider.IsEnabled = (this.TodoList.IsItemClickEnabled == false) ? true : false;
        }

        private async void Validate_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressRing.IsActive = true;
            foreach(ToDo item in this.TodoList.SelectedItems)
            {
                this.tasklist.Remove(item);
                await this.OneNoteAPI.DeleteNote(item.Url);
            }
            this.TodoList.IsItemClickEnabled = true;
            this.TodoList.SelectionMode = ListViewSelectionMode.Single;
            this.Valider.IsEnabled = false;
            this.ProgressRing.IsActive = false;
        }
    }
}
