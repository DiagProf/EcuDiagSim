using System.Collections.Generic;

namespace EcuDiagSim.App.Interfaces
{
    public interface ILocalizationService
    {
        IEnumerable<string> AvailableLanguages { get; }

        void Initialize();

        /// <summary>
        /// </summary>
        /// <param name="language">BCP47 language tag</param>
        /// <returns>a </returns>
        bool IsAvailable(string language);

        string? GetLanguage();

        /// <summary>
        /// </summary>
        /// <param name="language">BCP47 language tag</param>
        /// <remarks>Restart the app to apply this change.</remarks>
        /// <exception cref="InvalidLanguageException"></exception>
        void SetLanguage(string language);
    }
}