using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExporter : Editor
{
    [MenuItem("Assets/나만의추출기/선택한 스프라이트 PNG로 저장")]
    public static void SaveSpriteToImage()
    {
        // 프로젝트 창에서 선택한 스프라이트를 가져옴
        Object[] selectedObjects = Selection.objects;

        foreach (Object obj in selectedObjects)
        {
            if (obj is Sprite sprite)
            {
                // 스프라이트의 텍스처 정보 가져오기
                Texture2D sourceTexture = sprite.texture;
                Rect spriteRect = sprite.rect;

                // 새로운 텍스처 생성 (픽셀 읽기/쓰기가 가능해야 함)
                Texture2D newTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
                Color[] pixels = sourceTexture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);

                newTexture.SetPixels(pixels);
                newTexture.Apply();

                // PNG 파일로 인코딩
                byte[] bytes = newTexture.EncodeToPNG();
                string path = Path.Combine(Application.dataPath, sprite.name + ".png");

                // 파일 저장
                File.WriteAllBytes(path, bytes);
                Debug.Log(sprite.name + " 추출 완료: " + path);
            }
        }
        AssetDatabase.Refresh(); // 프로젝트 창 새로고침
    }
}