using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Apex.MVVM;

namespace Example
{
    /// <summary>
    /// The main view model of the application.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            //  Add a few characters to begin with.
            Characters.Add("Homer");
            Characters.Add("Marge");
            Characters.Add("Bart");
            Characters.Add("Lisa");
            Characters.Add("Maggie");

            //  The addCharacterCommand calls 'DoAddCharacter' and is disabled by default.
            addCharacterCommand = new ViewModelCommand(DoAddCharacter, false);

            //  The deleteCharacterCommand calls 'DoDeleteCharacter' and is enabled by default.
            deleteCharacterCommand = new ViewModelCommand(DoDeleteCharacter, true);
        }

        /// <summary>
        /// Adds the character.
        /// </summary>
        private void DoAddCharacter()
        {
            //  Add the character.
            Characters.Add(NewCharacterName);

            //  Clear the new character name.
            NewCharacterName = string.Empty;
        }

        /// <summary>
        /// Deletes a character.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void DoDeleteCharacter(object parameter)
        {
            //  Remove the character that was provided.
            Characters.Remove((string)parameter);
        }

        /// <summary>
        /// The set of characters.
        /// </summary>
        ObservableCollection<string> characters = new ObservableCollection<string>();

        /// <summary>
        /// Gets the characters.
        /// </summary>
        /// <value>The characters.</value>
        public ObservableCollection<string> Characters
        {
            get { return characters; }
        }


        /// <summary>
        /// The new character name notifying property.
        /// </summary>
        private NotifyingProperty NewCharacterNameProperty =
          new NotifyingProperty("NewCharacterName", typeof(string), default(string));

        /// <summary>
        /// Gets or sets the new name of the character.
        /// </summary>
        /// <value>The new name of the character.</value>
        public string NewCharacterName
        {
            get { return (string)GetValue(NewCharacterNameProperty); }
            set
            {
                SetValue(NewCharacterNameProperty, value);

                //  We enable the add character command only if we have a non-empty name.
                addCharacterCommand.CanExecute = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// The add character command.
        /// </summary>
        private ViewModelCommand addCharacterCommand;

        /// <summary>
        /// Gets the add character command.
        /// </summary>
        /// <value>The add character command.</value>
        public ViewModelCommand AddCharacterCommand
        {
            get { return addCharacterCommand; }
        }

        /// <summary>
        /// The delete character command.
        /// </summary>
        private ViewModelCommand deleteCharacterCommand;

        /// <summary>
        /// Gets the delete character command.
        /// </summary>
        /// <value>The delete character command.</value>
        public ViewModelCommand DeleteCharacterCommand
        {
            get { return deleteCharacterCommand; }
        }       
    }
}
