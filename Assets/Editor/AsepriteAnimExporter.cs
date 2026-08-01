using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D.Aseprite;
using System.Reflection;

public class AsepriteAnimExporter
{
    [MenuItem("Assets/Aseprite - 원클릭 폴더별 애니메이션 익스포트", false, 20)]
    public static void ExportSelectedAsepriteAnimations()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0) return;

        int successCount = 0;

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            string ext = Path.GetExtension(assetPath).ToLower();
            if (ext != ".ase" && ext != ".aseprite") continue;

            AsepriteImporter importer = AssetImporter.GetAtPath(assetPath) as AsepriteImporter;
            if (importer == null) continue;

            // 1. 유니티 프로젝트 기준 상대 경로 계산
            string parentDirectory = Path.GetDirectoryName(assetPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
            string targetExportDirectory = Path.Combine(parentDirectory, fileNameWithoutExtension).Replace("\\", "/");

            // 2. 유니티 내부에 폴더 생성
            if (!AssetDatabase.IsValidFolder(targetExportDirectory))
            {
                AssetDatabase.CreateFolder(parentDirectory, fileNameWithoutExtension);
            }

            // ★ 문제 해결 핵심: 유니티 내부 API가 요구하는 PC의 '절대 경로'를 계산합니다.
            string projectRootPath = Path.GetDirectoryName(Application.dataPath);
            string absoluteTargetDirectory = Path.Combine(projectRootPath, targetExportDirectory).Replace("\\", "/");

            AsepriteImporter[] importersToExport = new AsepriteImporter[] { importer };

            System.Type importUtilitiesType = typeof(AsepriteImporter).Assembly.GetType("UnityEditor.U2D.Aseprite.ImportUtilities");

            if (importUtilitiesType != null)
            {
                MethodInfo exportMethod = null;

                MethodInfo[] methods = importUtilitiesType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var method in methods)
                {
                    if (method.Name == "ExportAnimationAssets" && method.GetParameters().Length == 4)
                    {
                        exportMethod = method;
                        break;
                    }
                }

                if (exportMethod != null)
                {
                    // 상대 경로(targetExportDirectory) 대신 절대 경로(absoluteTargetDirectory)를 전달!
                    exportMethod.Invoke(null, new object[] { absoluteTargetDirectory, importersToExport, true, true });
                    successCount++;
                }
                else
                {
                    Debug.LogError("[Aseprite 툴] 매개변수가 4개인 ExportAnimationAssets 메서드를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("[Aseprite 툴] ImportUtilities 클래스를 찾을 수 없습니다.");
            }
        }

        if (successCount > 0)
        {
            AssetDatabase.Refresh();
            Debug.Log($"[Aseprite 툴] 총 {successCount}개의 캐릭터가 각각의 폴더로 '딸깍' 분리 추출 완료되었습니다!");
        }
        else
        {
            Debug.LogWarning("[Aseprite 툴] 선택된 파일 중 Aseprite 원본 파일(.ase / .aseprite)이 없거나 추출에 실패했습니다.");
        }
    }

    [MenuItem("Assets/Aseprite - 원클릭 폴더별 애니메이션 익스포트", true)]
    public static bool ValidateExportSelectedAsepriteAnimations()
    {
        if (Selection.activeObject == null) return false;
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string ext = Path.GetExtension(assetPath).ToLower();
        return (ext == ".ase" || ext == ".aseprite");
    }
}