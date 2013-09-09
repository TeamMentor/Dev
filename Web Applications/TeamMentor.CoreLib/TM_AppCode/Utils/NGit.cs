using FluentSharp.Git;

namespace TeamMentor.CoreLib
{
    public class NGit
    {
        public bool isRepository(string path)
        {
            return path.isGitRepository();
        }
    }
}
