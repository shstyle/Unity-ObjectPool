using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreatorManager : Singleton<CreatorManager> {

	public enum CreatorType { GameMonster, GameObject , UI, Environment, Effect,  }
	public List<CreatorModel> Creators = new List<CreatorModel>();

	bool cached = false;
	Dictionary<CreatorType, Creator> Cache = new Dictionary<CreatorType, Creator>();
	[System.Serializable]
	public class CreatorModel
	{
		public string creatorName;
		public CreatorType creatoeType;
		public Creator creator;
	}


	public void Init()
	{
		if(cached == false)
		{
			for(int i = 0 ;  i < Creators.Count ; i  ++)
				Cache.Add(Creators[i].creatoeType, Creators[i].creator);
			cached = true;
		}
	}
	public Creator GetCreator(CreatorType type)
	{
		if(cached == false) 
		{
			Init();
		}
		return Cache[type];
	}

}
