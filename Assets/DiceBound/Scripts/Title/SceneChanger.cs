using UnityEngine;
using UnityEngine.SceneManagement;

// �� �̵��� ���� �ݵ�� �ʿ���!

namespace DiceBound
{
    public class SceneChanger : MonoBehaviour
    {
        // ��ư �̺�Ʈ���� �� �Լ��� ��� �� �� �ְ� public���� ����Ӵϴ�.
        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}