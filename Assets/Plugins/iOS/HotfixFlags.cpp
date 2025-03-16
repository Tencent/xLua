#include <stddef.h>
#include <stdlib.h>
#include <string.h>

bool* xlua_hotfix_flags = NULL;
int xlua_hotfix_flags_len = 0;

extern "C"
{
    int xlua_get_hotfix_flag(int idx)
    {
        if (idx >= xlua_hotfix_flags_len)
        {
            return 0;
        }
        else
        {
            return xlua_hotfix_flags[idx];
        }
    }

    void xlua_set_hotfix_flag(int idx, int flag)
    {
        if (idx >= xlua_hotfix_flags_len)
        {
            bool* new_hotfix_flags = (bool*)malloc(idx + 1);

            if (xlua_hotfix_flags == NULL)
            {
                memset(new_hotfix_flags, 0, (idx + 1));
                xlua_hotfix_flags = new_hotfix_flags;
            }
            else
            {
                memcpy(new_hotfix_flags, xlua_hotfix_flags, xlua_hotfix_flags_len);
                memset(new_hotfix_flags + xlua_hotfix_flags_len, 0, (idx + 1 - xlua_hotfix_flags_len));
                bool* tmp = xlua_hotfix_flags;
                xlua_hotfix_flags = new_hotfix_flags;
                free(tmp);
            }

            xlua_hotfix_flags_len = idx + 1;
        }

        xlua_hotfix_flags[idx] = flag;
    }
}
