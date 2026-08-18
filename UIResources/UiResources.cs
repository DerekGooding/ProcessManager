namespace ProcessManager.UiResources
{
    internal static class UiResource
    {
        public const string Logo = @"
                                ..:                     
                                .-:                     
               -====             .*         =--=       
               :--+=-            =#+        ====        
                 -:: -           :%= ++    +*+##*       
                   :+           :**-+-   ++#%=*++       
                     :#+         --=-  -*+=++           
                       #+        :--- : ===+=*          
                       - **     ==+= =  ==:=            
                 - --  :===#-+++*%#:+**#%%#::---++:- .::
             :-         -*+-*-=%%@%*%%%%+#+++ =--==:    
                            :*##%#%%%++   +=--+=*+=---=-
                               **#.-#=-==.=+*#+==       
                               =+= .:+*#%#+:.:          
                    ##=-+#*    -= . .--.#-#.: .-        
                *+#**%@#*      :=*= .***=+*#%*+ .       
              *#=*-=#++       =-=.=  +##%%++#%%%%#.++   
             #*#%%#@#*         --.   +##%%%#+%**@@#*+#+ 
            ###%%%%#           ::.   %#%%%#  ##%%%##*  
            ##%%%              :-:.  %#%%      ##      
                               =-:. #%%%              
                               =-:  *%%              
                                :
";

        public static readonly string[] MenuOptions =
            [
                "Enter: Processes Menu",
            "Esc: Exit",
        ];

        public static readonly string[] FilterOptions =
           [
                "1. Filter by Name",
            "2. Filter by PID",
            "3. Filter by Memory",
       ];

        public static readonly string[] ProcessOptions =
           [
                "1. Kill Process",
            "2. Close main process window",
            "3. Open process file directory",
            "4. Change priority of process"
           ];

        public static readonly string[] ChangePriorityOptions =
           [
                "1. RealTime",
            "2. High",
            "3. AboveNormal",
            "4. Normal",
            "5. BelowNormal",
            "6. Idle",
       ];
    }
}
