-- Tencent is pleased to support the open source community by making xLua available.
-- Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
-- Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
-- http://opensource.org/licenses/MIT
-- Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

local speed = 10
local lightCpnt = nil

function start()
	print("lua start...")
	print("injected object", lightObject)
	lightCpnt= lightObject:GetComponent(typeof(CS.UnityEngine.Light))
end

function update()
	local r = CS.UnityEngine.Vector3.up * CS.UnityEngine.Time.deltaTime * speed
	self.transform:Rotate(r)
	lightCpnt.color = CS.UnityEngine.Color(CS.UnityEngine.Mathf.Sin(CS.UnityEngine.Time.time) / 2 + 0.5, 0, 0, 1)
end

function ondestroy()
    print("lua destroy")
end

