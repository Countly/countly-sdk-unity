using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Countly 
{
  public class Profile {

	public string name {get; set;}
	public string username {get; set;}
	public string email {get; set;}
	public string organisation {get; set;}
	public string phone {get; set;}
	public string picture {get; set;}
	public string gender {get; set;}
	public string byear {get; set;}

	public Dictionary<string, string> custom;

	
	public void Init() {
	  name = "";
	  username = "";
	  email = "";
	  organisation = "";
	  phone = "";
	  picture = "";
	  gender = "";
	  byear = "";

	  custom = new Dictionary<string, string>();
	}


	public StringBuilder JSONSerializeProfile()
	  {
		StringBuilder builder = new StringBuilder();
		// open metrics object
		builder.Append("{");
			
		builder.Append("\"name\":");
		Utils.JSONSerializeString(builder, name);
			
		builder.Append(",\"username\":");
		Utils.JSONSerializeString(builder, username);
			
		builder.Append(",\"email\":");
		Utils.JSONSerializeString(builder, email);

		builder.Append(",\"organisation\":");
		Utils.JSONSerializeString(builder, organisation);
					
		builder.Append(",\"phone\":");
		Utils.JSONSerializeString(builder, phone);

		builder.Append(",\"picture\":");
		Utils.JSONSerializeString(builder, picture);
		
		builder.Append(",\"gender\":");
		Utils.JSONSerializeString(builder, gender);

		builder.Append(",\"byear\":");
		Utils.JSONSerializeString(builder, byear);

		  if (custom != null && custom.Count > 0) {
			builder.Append("\"custom\":{");
			  foreach (KeyValuePair<string, string> pair in custom) {
			    builder.Append("\"" + pair.Key + "\":\"" + pair.Value + "\",");

			}
			builder.Length = builder.Length-1;
			builder.Append("}");
		  }
	    
		

		builder.Append("}");

		return builder;
	  }
  }


}
