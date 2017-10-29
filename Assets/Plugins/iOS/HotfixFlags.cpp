#include <stddef.h>
#include <stdlib.h>

int* xlua_hotfix_flags = NULL;
int xlua_hotfix_flags_len = 0;

extern "C" {

int xlua_get_hotfix_flag(int idx) {
	if (idx >= xlua_hotfix_flags_len) {
		return 0;
	} else {
		return xlua_hotfix_flags[idx];
	}
}

void xlua_set_hotfix_flag(int idx, int flag) {
	int i = 0;
        int* new_hotfix_flags = NULL;
	if (idx >= xlua_hotfix_flags_len) {
		if (xlua_hotfix_flags == NULL) {
			xlua_hotfix_flags = (int*)malloc((idx + 1) * sizeof(int));
		} else {
			new_hotfix_flags = (int*)realloc(xlua_hotfix_flags, (idx + 1) * sizeof(int));
                        if (NULL == new_hotfix_flags) { // just skip operation
                            return;
                        }
                        xlua_hotfix_flags = new_hotfix_flags;
		}
		for(i = xlua_hotfix_flags_len; i < (idx + 1); i++) {
			xlua_hotfix_flags[i] = 0;
		}
                xlua_hotfix_flags_len = idx + 1;
	} 
	xlua_hotfix_flags[idx] = flag;
}
}
