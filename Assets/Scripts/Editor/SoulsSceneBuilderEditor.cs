using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SoulsALaCarte.Builder;

namespace SoulsALaCarte.Editor
{
    public static class SoulsSceneBuilderEditor
    {
        [MenuItem("Souls A La Carte/Construir Escena Completa (Build Scene)")]
        public static void BuildSceneMenu()
        {
            Undo.SetCurrentGroupName("Build Souls A La Carte Scene");
            int group = Undo.GetCurrentGroup();

            SoulsGameBuilder.BuildCompleteScene();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Undo.CollapseUndoOperations(group);

            EditorUtility.DisplayDialog("Souls A La Carte", 
                "¡Escena construida con éxito!\n\nSe han creado los vagones de Cocina, Comedor, Estaciones, Personajes (Francis, Giuseppe, Migafees) y la Interfaz completa con HUD y Menús.\n\nPulsa PLAY para jugar.", 
                "¡Genial!");
        }

        [MenuItem("Souls A La Carte/Resetear Progreso Guardado")]
        public static void ResetProgressMenu()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Souls A La Carte", "El progreso y las monedas guardadas han sido reiniciadas.", "OK");
        }
    }
}
