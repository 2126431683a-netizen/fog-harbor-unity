using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 一键生成游戏场景：菜单「雾港疑云/生成游戏场景」
public static class GameBuilder
{
    [MenuItem("FogHarbor/Build Game Scene")]
    [MenuItem("雾港疑云/生成游戏场景")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("Game");
        go.AddComponent<Game>();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true) };
        Debug.Log("雾港疑云：场景已生成 → Assets/Scenes/Game.unity，直接点 Play 即可");
    }
}
