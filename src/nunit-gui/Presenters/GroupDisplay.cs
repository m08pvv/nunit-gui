﻿// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NUnit.Gui.Presenters
{
    using Views;
    using Model;
    using Engine;
    using NUnit.UiKit.Elements;

    public abstract class GroupDisplay : DisplayStrategy
    {
        protected string _groupBy;
        protected TestGrouping _grouping;

        #region Construction and Initialization

        public GroupDisplay(ITestTreeView view, ITestModel model)
            : base(view, model) 
        {
            _view.GroupBy.Enabled = true;
            _view.GroupBy.SelectionChanged += OnGroupByChanged;
        }

        #endregion

        /// <summary>
        /// Post a test result to the tree, changing the treeNode
        /// color to reflect success or failure. Overridden here
        /// to allow for moving nodes from one group to another
        /// based on the result of running the test.
        /// </summary>
        protected override void OnTestFinished(TestNode result)
        {
            base.OnTestFinished(result);

            _grouping.AdjustGroupsBasedOnTestResult(result);
        }

        protected abstract void OnGroupByChanged();

        protected TestGrouping CreateTestGrouping(string groupBy)
        {
            switch (_groupBy)
            {
                case "OUTCOME":
                    return new GroupByOutcome(this);
                case "DURATION":
                    return new GroupByDuration(this);
                case "CATEGORY":
                    return new GroupByCategory(this);
            }

            return null;
        }

        protected void UpdateDisplay()
        {
            this.ClearTree();
            TreeNode topNode = null;
            foreach (var group in _grouping)
            {
                var treeNode = MakeTreeNode(group, true);
                group.TreeNode = treeNode;
                treeNode.Expand();
                if (group.Count > 0)
                {
                    // We use Group.TreeNode for its side effect
                    // of updating the test count on the node.
                    // TODO: Move this responsibility to the strategy.
                    _view.Tree.Add(group.TreeNode);
                    if (topNode == null)
                        topNode = treeNode;
                }
            }
            if (topNode != null)
                topNode.EnsureVisible();
        }
    }
}