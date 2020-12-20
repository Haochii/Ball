using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CSVManager : MonoBehaviour
{
	public static CSVManager Instance;
	public List<List<string>> GlobalLists;
	public List<List<string>> SkillLists;
	public List<List<string>> BuffLists;
	public List<List<string>> BallTypeLists;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		//lists = MyReadCSV.Read(Application.streamingAssetsPath + "/test.csv", Encoding.Default);// 打包完地址
		//lists = MyReadCSV.Read(Application.dataPath + "/test.csv", Encoding.Default);//            打包前测试地址
		GlobalLists = MyReadCSV.Read(Application.dataPath + "/Files/global.csv", Encoding.Default);
		SkillLists = MyReadCSV.Read(Application.dataPath + "/Files/pinballBasis.csv", Encoding.Default);
		BuffLists = MyReadCSV.Read(Application.dataPath + "/Files/skills.csv", Encoding.Default);
		BallTypeLists = MyReadCSV.Read(Application.dataPath + "/Files/wallRandom.csv", Encoding.Default);
	}
}
