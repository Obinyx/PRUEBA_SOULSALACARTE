using UnityEngine;

namespace SoulsALaCarte.Builder
{
    [DefaultExecutionOrder(-1000)]
    public class SoulsBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (FindAnyObjectByType<Core.GameManager>() == null)
            {
                SoulsGameBuilder.BuildCompleteScene();
            }
        }
    }
}
