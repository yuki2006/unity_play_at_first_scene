using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAtFirstScene
{
    // PlayerPrefsのキー名
    public const string UnitySceneList = "_UNITY_SCENE_LIST";
    public const string UnityActiveScenePath = "_UNITY_ACTIVE_SCENE_PATH";


    [InitializeOnLoadMethod]
    public static void SetEvent()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }


    [MenuItem("最初のシーンから実行/ビルド設定の最初から")]
    public static void EnteredPlayMode()
    {
        // シーンの変更時に確認ダイアログを出す。
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        List<string> list = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            list.Add(SceneManager.GetSceneAt(i).path);
        }

// 現在のシーン情報を保存する
        SetScenePaths(list.ToArray(), SceneManager.GetActiveScene());

        //　ビルド設定の最初のシーンを開く 
        if (EditorBuildSettings.scenes.Length > 0)
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        }

        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        // 4通りのイベントで呼ばれる
        if (change != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }

        // ゲーム再生が終わったのでシーンの復元をする
        var scenePaths = GetScenePaths();
        if (scenePaths.Length > 0)
        {
            var activeScenePath = GetActiveScenePath();

            EditorSceneManager.OpenScene(scenePaths[0]);
            for (int i = 1; i < scenePaths.Length; i++)
            {
                // 2個以上のシーンが有る場合は
                var scene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Additive);
                if (scene.path == activeScenePath)
                {
                    // 複数シーンがあったときにアクティブに復元するようにする
                    SceneManager.SetActiveScene(scene);
                }
            }
        }

        // 復元後はセーブデータからも削除する
        RemoveScenePaths();
    }

    public static void SetScenePaths(string[] value, Scene activeScene)
    {
        string str = String.Join(",", value);

        PlayerPrefs.SetString(UnitySceneList, str);
        PlayerPrefs.SetString(UnityActiveScenePath, activeScene.path);
        PlayerPrefs.Save();
    }

    public static string[] GetScenePaths()
    {
        var s = PlayerPrefs.GetString(UnitySceneList);
        if (s == "")
        {
            return new string[0];
        }

        return s.Split(',');
    }

    public static string GetActiveScenePath()
    {
        return PlayerPrefs.GetString(UnityActiveScenePath);
    }

    public static void RemoveScenePaths()
    {
        PlayerPrefs.DeleteKey(UnitySceneList);
        PlayerPrefs.DeleteKey(UnityActiveScenePath);
        PlayerPrefs.Save();
    }
}