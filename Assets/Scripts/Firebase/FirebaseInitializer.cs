using Firebase;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    private async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("🔥 Firebase initialized successfully");

            // רק לוג — אין טעינת שחקן פה יותר
        }
        else
        {
            Debug.LogError("❌ Firebase init failed: " + dependencyStatus);
        }
    }
}