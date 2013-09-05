using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace TeamMentor.CoreLib
{
    [DataContract]    
	public class NewUser
	{
        [DataMember][StringLength(30)]              public string   Company     { get; set; }
        [DataMember][StringLength(30)]              public string   Country     { get; set; }        
        [DataMember][StringLength(30)]              public string   Firstname   { get; set; }
		[DataMember][StringLength(30)]              public string   Lastname    { get; set; }        
        [DataMember][StringLength(30)]              public string   Note        { get; set; }		
		[DataMember][Required][StringLength(30)]    public string   Password    { get; set; }
		[DataMember][StringLength(30)]              public string   State       { get; set; }		
		[DataMember][StringLength(30)]              public string   Title       { get; set; }
		[DataMember][Required][StringLength(30)]    public string   Username    { get; set; }

        [DataMember][Required][StringLength(50)]
        [RegularExpression(ValidationRegex.Email)]  public string   Email       { get; set; }

        [DataMember]                                public int      GroupId     { get; set; }
	}
}